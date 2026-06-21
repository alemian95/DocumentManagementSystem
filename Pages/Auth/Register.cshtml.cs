using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocumentManagementSystem.Pages.Auth;

public class RegisterModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IConfiguration configuration,
        ILogger<RegisterModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _logger = logger;
    }

    public class InputModel
    {
        [Required(ErrorMessage = "L'email è obbligatoria.")]
        [EmailAddress(ErrorMessage = "Inserisci un indirizzo email valido.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La password è obbligatoria.")]
        [StringLength(100, ErrorMessage = "La {0} deve essere lunga almeno {2} e al massimo {1} caratteri.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Conferma password")]
        [Compare("Password", ErrorMessage = "La password e la password di conferma non corrispondono.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    // Proprietà per esporre lo stato della registrazione alla View HTML
    public bool IsRegistrationEnabled { get; private set; }

    public void OnGet()
    {
        // Leggiamo il flag dal file appsettings.json (default a true se manca)
        IsRegistrationEnabled = _configuration.GetValue<bool>("DmsSettings:EnableRegistration", false);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Verifica di sicurezza anche sul metodo POST (evita bypass tramite strumenti come Postman)
        IsRegistrationEnabled = _configuration.GetValue<bool>("DmsSettings:EnableRegistration", false);
        if (!IsRegistrationEnabled)
        {
            ModelState.AddModelError(string.Empty, "Le registrazioni sono momentaneamente disabilitate dall'amministratore.");
            return Page();
        }

        if (!ModelState.IsValid)
    {
        foreach (var modelStateKey in ModelState.Keys)
        {
            var value = ModelState[modelStateKey];
            foreach (var error in value.Errors)
            {
                _logger.LogWarning($"Errore sul campo {modelStateKey}: {error.ErrorMessage}");
            }
        }
    }

        if (ModelState.IsValid)
        {
            // Creiamo l'istanza dell'utente. Usiamo l'email sia come Username che come Email.
            var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
            
            // Il CreateAsync si occupa di fare l'hashing sicuro della password prima di salvarla su SQLite
            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("L'utente ha creato un nuovo account con password.");

                // Dopo la registrazione, logghiamo direttamente l'utente e mandiamolo in Dashboard
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToPage("/App/Dashboard");
            }

            // Se Identity restituisce errori (es. utente già esistente, password troppo semplice), li mostriamo nel form
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // Se siamo qui, qualcosa è andato storto, ricarichiamo la pagina
        return Page();
    }
}