/*
Considerations about the safety of optimization #1 (DefaultIfEmpty not producing a new subquey, but introducing a SELECT NULL AS Empty anyway)
- NOT OPTIMIZED: GROUP BY must not be optimized (see integration tests below).
- OPTIMIZED: ORDER BY doesn't pose a problem because in the empty row case, it doesn't do anything
- OPTIMIZED: WHERE is moved to the ON condition, which should always work since it's executed "before"/during the join
- OPTIMIZED: Multiple joins in the table moved to the LEFT JOIN don't pose a problem because re-linq nests joins correctly (joins nested in the model are also nested in the generated SQL because their ON conditions are generated in the right places)
- OPTIMIZED: Subqueries in the table moved to the LEFT JOIN don't pose a problem because they continue to be executed prior to the LEFT JOIN
- NOT OPTIMIZED: Some expressions in SELECT _do_ pose a problem because they behave differently if executed prior to/after the join. E.g., constants: should become "default" (NULL) when executed before the DefaultIfEmpty(), but they will keep their constant value when evaluated after the LEFT JOIN.
-- Aggregation expressions (see integration tests below) in expressions are not safe, must not be optimized: At least for COUNT(*), the result differs.
-- Same with ROW_NUMBER().
-- Potential solutions:
-- 1 - Use the flag indicating whether a "default" value was selected or not that we need to fix bug RMLNQSQL-56 anyway. Problem: This either bloats the projection of the subquery or it bloats the usages of this projection. Can be combined with #2 to reduce bloating.
-- 2 - Identify unsafe SELECT expressions and omit the optimization in those cases. Problem: Will need to be a complex visitor and take extensibility into account (e.g., custom expressions, custom SQL functions, etc.)-
-- 3 - Limit to absolutely safe subset of SELECT expressions. (Absolutely required: Reference to selected table/joined tables. Nice to have: Member references and compound expressions.)
- NOT OPTIMIZED: RowNumberSelector is a problem because the previous value of ROW_NUMBER is no longer valid after the DefaultIfEmpty. Therefore, do not optimize. (Comment: Potential optimization variant: Simply clear RowNumberSelector. However, not worth the special case.)
- NOT OPTIMIZED: Same with TOP. Do not optimize.
- OPTIMIZED: DISTINCT is no problem.
*/

-- Test Cases illustrating potential problems

-- Items.GroupBy(x => x.Key, g => new { g.Key, g.Max(x => x.Item), g.Count(x => x.Item) }).DefaultIfEmpty() => Must not be optimized, different results!
SELECT Items.[Key], MAX(Items.Item), COUNT(*)
FROM (SELECT NULL AS Empty) AS Empty
  OUTER APPLY (SELECT 1 AS Item, 'X' AS [Key] WHERE 1 = 0) AS Items
GROUP BY Items.[Key]

SELECT q.[Key], q.Value1, q.Value2
FROM (SELECT NULL AS Empty) AS Empty
  OUTER APPLY ( SELECT  Items.[Key] AS [Key], MAX(Items.Item) As Value1, COUNT(*) As Value2 FROM (SELECT 1 AS Item, 'X' AS [Key] WHERE 1 = 0) AS Items
GROUP BY Items.[Key]
) AS q

-- Items.Max(x => x.Item).DefaultIfEmpty() => May be optimized.
SELECT MAX(Items.Item)
FROM (SELECT NULL AS Empty) AS Empty
  OUTER APPLY  (SELECT 1 AS Item, 'X' AS [Key] WHERE 1 = 0) AS Items

SELECT q.Value
FROM (SELECT NULL AS Empty) AS Empty
  OUTER APPLY  (SELECT MAX(Items.Item) AS Value FROM (SELECT 1 AS Item, 'X' AS [Key] WHERE 1 = 0) AS Items) AS q

-- Items.Count(x => x.Item).DefaultIfEmpty() => Must not be optimized, different results!
SELECT COUNT(*)
FROM (SELECT NULL AS Empty) AS Empty
  OUTER APPLY  (SELECT 1 AS Item, 'X' AS [Key] WHERE 1 = 0) AS Items

SELECT q.Value
FROM (SELECT NULL AS Empty) AS Empty
  OUTER APPLY  (SELECT COUNT(*) AS Value FROM (SELECT 1 AS Item, 'X' AS [Key] WHERE 1 = 0) AS Items) AS q

-- Items.Where(x => x.Customer != null).Select(x => new { x.Customer.Name, x.Customer.ID }).DefaultIfEmpty() => May be optimized because member reference correctly returns null even when executed on the empty row. Problem that compound expression is not null (only its members are) is a different issue.
SELECT Customers.Name, Customers.ID
FROM (SELECT NULL AS Empty) AS Empty
  LEFT JOIN (SELECT 1 AS Item, 'X' AS [Key], 10 AS CustomerID WHERE 1 = 0) AS Items ON Items.CustomerID IS NOT NULL
  LEFT JOIN (SELECT 10 AS ID, 'Some Customer' AS Name) Customers ON Customers.ID = Items.CustomerID

SELECT q.Value1, q.Value2
FROM (SELECT NULL AS Empty) AS Empty
  OUTER APPLY (
    SELECT Customers.Name AS Value1, CUstomers.Id AS Value2 FROM (SELECT 1 AS Item, 'X' AS [Key], 10 AS CustomerID WHERE 1 = 0) AS Items
      LEFT JOIN (SELECT 10 AS ID, 'Some Customer' AS Name) Customers ON Customers.ID = Items.CustomerID
    WHERE Items.CustomerID IS NOT NULL
  ) AS q

-- Items.Select(x =>"Hugo").DefaultIfEmpty() => Must not be optimized (or needs complex scenario; needs to return null in empty row case).

-- Integration test for multiple froms with DefaultIfEmpty after each other
-- from c in Customers
-- from o in c.Orders.DefaultIfEmpty()
-- from e in c.Employees.DefaultIfEmpty()
-- select new { c.ID, o.ID, e.ID }

-- Expected:
--SELECT c.ID, o.ID, e.ID
--FROM
--  [Customer] AS c
--  LEFT JOIN [Order] AS o ON o.CustomerID = c.ID
--  LEFT JOIN [Employee] AS  e ON e.CustomerID = c.ID

-- Items.Skip(10).Take(5).DefaultIfEmpty()

SELECT q.[Key]
FROM (SELECT NULL AS [Empty]) AS [Empty]
  LEFT JOIN (
      SELECT Items.Item AS [Key], ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) AS Value
      FROM (SELECT 1 AS Item, 'X' AS [Key] WHERE 1 = 0) AS Items
    ) AS q
    ON q.Value > 10 AND q.Value <= 10 + 5

SELECT q2.[Key]
FROM (SELECT NULL AS [Empty]) AS [Empty]
  OUTER APPLY (
    SELECT q.[Key]
    FROM (
      SELECT Items.Item AS [Key], ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) AS Value
      FROM (SELECT 1 AS Item, 'X' AS [Key] WHERE 1 = 0) AS Items
    ) AS q
    WHERE q.Value > 10 AND q.Value <= 10 + 5
  ) AS q2

-- Not expressable: DefaultIfEmpty directly on ROW_NUMBER

SELECT Items.Item AS [Key], ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) AS Value
FROM (SELECT NULL AS [Empty]) AS [Empty]
  OUTER APPLY (SELECT 1 AS Item, 'X' AS [Key] WHERE 1 = 0) AS Items

SELECT q2.[Key], q2.[Value]
FROM (SELECT NULL AS [Empty]) AS [Empty]
  OUTER APPLY (
      SELECT Items.Item AS [Key], ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) AS Value
      FROM (SELECT 1 AS Item, 'X' AS [Key] WHERE 1 = 0) AS Items
  ) AS q2

-- Items.Skip(10).DefaultIfEmpty().Take(5) => Must not be optimized, different results!

SELECT q.[Key]
FROM (SELECT NULL AS [Empty]) AS [Empty]
  LEFT JOIN (
      SELECT Items.Item AS [Key], ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) AS Value
      FROM (SELECT 1 AS Item, 'X' AS [Key] WHERE 1 = 0) AS Items
    ) AS q
    ON q.Value > 10
  WHERE q.Value < 10 + 5  

SELECT TOP 5 q2.[Key]
FROM (SELECT NULL AS [Empty]) AS [Empty]
  OUTER APPLY (
    SELECT q.[Key]
    FROM (
      SELECT Items.Item AS [Key], ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) AS Value
      FROM (SELECT 1 AS Item, 'X' AS [Key] WHERE 1 = 0) AS Items
    ) AS q
    WHERE q.Value > 10
  ) AS q2

-- Items.Take(0).DefaultIfEmpty() => Must not be optimized, different results!

SELECT TOP 0 Items.Item
FROM (SELECT NULL AS [Empty]) AS [Empty]
  OUTER APPLY (SELECT 1 AS Item, 'X' AS [Key] WHERE 1 = 0) AS Items

SELECT q2.Item
FROM (SELECT NULL AS [Empty]) AS [Empty]
  OUTER APPLY (
    SELECT TOP 0 Items.Item
    FROM (SELECT 1 AS Item, 'X' AS [Key] WHERE 1 = 0) AS Items
  ) AS q2

-- Items.Distinct().DefaultIfEmpty() => May be optimized.

SELECT DISTINCT Items.Item
FROM (SELECT NULL AS [Empty]) AS [Empty]
  OUTER APPLY (SELECT 1 AS Item, 'X' AS [Key] WHERE 1 = 0) AS Items

SELECT q2.Item
FROM (SELECT NULL AS [Empty]) AS [Empty]
  OUTER APPLY (
    SELECT DISTINCT Items.Item
    FROM (SELECT 1 AS Item, 'X' AS [Key] WHERE 1 = 0) AS Items
  ) AS q2

/*
Considerations about the safety of optimization #2 (elimination of subquery existing in original LINQ expression due to DefaultIfEmpty being in an additional from clause)

Example
from x in Items1
from y in Items2. ... .DefaultIfEmpty()
select new { x.Name, y.Data }

=> Naive translation
SELECT x.Name, q.Value
FROM Items1 x
  CROSS APPLY (
    SELECT y.Data AS Value
    FROM (SELECT NULL AS Empty) AS Empty
      LEFT JOIN Items2 y ON ...
  ) q

=> Eliminate subquery for Items2.DefaultIfEmpty() in second from clause in order to produce:
SELECT x.Name, y.Data
FROM Items1 x 
  LEFT JOIN Items2 y ON ...

- Concentrate on cases worth optimizing: NOT OPTIMIZED: GROUP BY, ORDER BY, TOP, DISTINCT, RowNumberSelector, WHERE, more than 1 table, set operations
- The SELECT expression values are moved from the subquery to the outer query. Therefore, it is (may be) evaluated more often (once for every result row, after the cartesian product) than before (once for every inner row, before the cartesian product). 
-- Therefore, every expression where this may produce different values (newguid(), getdate(), aggregations, ROW_NUMBER()) or have negative performance impacts (subquery) must stop this optimization from happending.
-- Expressions where we don't care how often they are evaluated: Table references, member references, constants (literal/parameters), all operators (including like, case, binary, unary, exists, etc.).
- NOT OPTIMIZED: Dependent subqueries, due to limitation of LEFT JOIN (must not access items from previous tables).

Example for newguid(): The second variant produces more different GUIDs than the first one.

  SELECT x.Name, q.Value, q.Value2
  FROM Items1 x
    CROSS APPLY (
      SELECT y.Data AS Value, newguid() AS Value2
      FROM (SELECT NULL AS Empty) AS Empty
        LEFT JOIN Items2 y ON ...
    ) q

  =>

  SELECT x.Name, y.Data, newguid()
  FROM Items1 x 
    LEFT JOIN Items2 y ON ...

*/

-- Test Cases illustrating potential problems