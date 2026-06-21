using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocumentManagementSystem.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<LoginModel> _logger;
    private readonly IConfiguration _configuration;
    public bool IsRegistrationEnabled { get; private set; }

    public LoginModel(
        SignInManager<IdentityUser> signInManager, 
        ILogger<LoginModel> logger,
        IConfiguration configuration
    )
    {
        _signInManager = signInManager;
        _logger = logger;
        _configuration = configuration;
        IsRegistrationEnabled = _configuration.GetValue<bool>("DmsSettings:EnableRegistration", false);
    }

    // InputModel definisce i campi necessari per il form di login
    public class InputModel
    {
        [Required(ErrorMessage = "L'email è obbligatoria.")]
        [EmailAddress(ErrorMessage = "Inserisci un indirizzo email valido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La password è obbligatoria.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Ricordami su questo dispositivo")]
        public bool RememberMe { get; set; }
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    [TempData]
    public string ErrorMessage { get; set; } = string.Empty;

    public void OnGet(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        // Se l'utente provava ad accedere a una pagina privata (es. /App/Dashboard),
        // .NET ci passa l'URL di provenienza così possiamo ridirigerlo lì dopo il login
        ReturnUrl = returnUrl ?? Url.Content("~/");
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/App/Dashboard");

        if (ModelState.IsValid)
        {
            // Identity usa l'Username per il login di default. 
            // Nel nostro caso impostiamo l'username uguale all'email (lo vedremo nella registrazione),
            // quindi passiamo Input.Email come primo parametro.
            var result = await _signInManager.PasswordSignInAsync(
                Input.Email, 
                Input.Password, 
                Input.RememberMe, 
                lockoutOnFailure: false); // Mettiamo false per evitare il blocco account temporaneo dopo X tentativi falliti

            if (result.Succeeded)
            {
                _logger.LogInformation("Utente connesso con successo.");
                return LocalRedirect(returnUrl);
            }
            
            // Se arriviamo qui, c'è stato un errore (password errata o utente inesistente)
            ModelState.AddModelError(string.Empty, "Tentativo di accesso non valido. Controlla email e password.");
            return Page();
        }

        // Se il modello non è valido (es. email non formattata bene), ricarica la pagina mostrando gli errori
        return Page();
    }
}