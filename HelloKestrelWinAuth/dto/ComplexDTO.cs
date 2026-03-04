using System.Data;

namespace HelloKestrelWinAuth.dto
{
    public class ComplexDto
    {
        public int Id { get; set; }
        public string name { get; set; } = "";
        public DataSet DataSet { get; set; } = new DataSet();
    }
}
