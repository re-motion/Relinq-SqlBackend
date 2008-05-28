using System.Collections.Generic;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Utilities;

namespace Remotion.Data.Linq.SqlGeneration
{
  public class SqlGenerationData
  {
    public SqlGenerationData()
    {
      FromSources = new List<IColumnSource> ();
      SelectEvaluations = new List<IEvaluation> ();
      OrderingFields = new List<OrderingField> ();
      Joins = new JoinCollection ();
      LetEvaluations = new List<LetData> ();
    }

    public List<IColumnSource> FromSources { get; private set; }
    public List<IEvaluation> SelectEvaluations { get; private set; }
    public ICriterion Criterion { get; set; }
    public bool Distinct { get; set; }
    public List<OrderingField> OrderingFields { get; private set; }
    public JoinCollection Joins { get; private set; }
    public List<LetData> LetEvaluations { get; private set; }
    public ParseContext ParseContext { get; set; }
    
    
  }
}