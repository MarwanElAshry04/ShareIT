using System.ComponentModel.DataAnnotations;

namespace ShareIT.Models
{
    /// <summary>
    /// The three sections of the public User Guide page. Display names are the
    /// headings rendered on that page.
    /// </summary>
    public enum VideoSection
    {
        [Display(Name = "How to File a New Case")] NewCase,
        [Display(Name = "How to Track Your Case")] TrackCase,
        [Display(Name = "ShareIT Videos")] ShareITVideos
    }
}