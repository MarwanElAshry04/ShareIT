using ShareIT.Models;

namespace ShareIT.Models.ViewModel
{
    /// <summary>
    /// The public User Guide page: every VideoSection in enum order, each with the
    /// videos an admin has uploaded into it.
    /// </summary>
    public class UserGuideViewModel
    {
        public List<GuideSectionViewModel> Sections { get; set; } = new();
    }

    public class GuideSectionViewModel
    {
        public VideoSection Section { get; set; }
        public string Heading { get; set; } = "";
        public string IconClass { get; set; } = "";
        public List<Video> Videos { get; set; } = new();
    }
}
