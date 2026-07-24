using System.ComponentModel.DataAnnotations;

namespace ShareIT.Models.ViewModel
{
    public class RoleFormViewModel
    {

        [Required, StringLength(256)]
        public string Name { get; set; }
    }
}
