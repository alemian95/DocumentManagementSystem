using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocumentManagementSystem.Pages.App;

[Authorize] // <-- Questo blocca l'accesso ai non autenticati!
public class DashboardModel : PageModel
{
    public void OnGet()
    {
    }
}