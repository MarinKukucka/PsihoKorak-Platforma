using PsihoKorak_Platforma.Models;

namespace PsihoKorak_Platforma.ViewModels
{
    public class MDViewModel
    {
        public int SessionId { get; set; }
        public string DateTime { get; set; }
        public string Duration { get; set; }
        public IEnumerable<Help> Helps { get; set; }
    }
}
