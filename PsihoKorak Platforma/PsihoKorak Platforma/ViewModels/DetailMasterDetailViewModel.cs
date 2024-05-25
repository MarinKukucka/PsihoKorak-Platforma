using PsihoKorak_Platforma.Models;

namespace PsihoKorak_Platforma.ViewModels
{
    public class DetailMasterDetailViewModel
    {
        public MDViewModel Sessions { get; set; }
        public IEnumerable<HelpsViewModelHelper> Helps { get; set; }
        public IEnumerable<Patient> Patients { get; set; }
        public IEnumerable<SessionType> SessionTypes { get; set; }
    }
}
