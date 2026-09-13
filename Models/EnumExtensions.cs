using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ShareIT.Models
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Friendly label for an enum value — uses [Display(Name)] when present,
        /// otherwise the raw enum name. Used to render TicketType across the UI now
        /// that the Category lookup table has been retired.
        /// </summary>
        public static string GetDisplayName(this Enum value)
        {
            var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            return member?.GetCustomAttribute<DisplayAttribute>()?.Name ?? value.ToString();
        }
    }
}
