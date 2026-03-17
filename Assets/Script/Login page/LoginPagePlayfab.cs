using MailKit.Net.Smtp;
using MimeKit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using System;
using UnityEngine.SceneManagement;
using MailKit.Security;

public class LoginPagePlayfab : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI MessageText;

    [Header("Login")]
    [SerializeField] Text TopLoginText;
    [SerializeField] InputField EmailLoginInput;
    [SerializeField] InputField PasswordLoginInput;
    [SerializeField] GameObject LoginPage;

    [Header("Register")]
    [SerializeField] Text TopRegisterText;
    [SerializeField] InputField UsernameRegisterInput;
    [SerializeField] InputField EmailRegisterInput;
    [SerializeField] InputField PasswordRegisterInput;
    [SerializeField] InputField PasswordConfirmationRegisterInput;
    [SerializeField] GameObject RegisterPage;

    [Header("Recovery")]
    [SerializeField] Text TopRecoveryText;
    [SerializeField] InputField EmailRecoveryInput;
    [SerializeField] GameObject RecoverySendEmail;
    [SerializeField] GameObject RecoveryPage;

    [Header("VerifMail")]
    [SerializeField] Text TopMailVerifText;
    [SerializeField] InputField CodeVerifInput;
    [SerializeField] GameObject CodeVerifConfirmation;
    [SerializeField] GameObject VerifMailPage;


    #region Button Fonctions
    public void OpenLoginPage()
    {
        LoginPage.SetActive(true);
        RegisterPage.SetActive(false);
        RecoveryPage.SetActive(false);
        VerifMailPage.SetActive(false);
    }

    public void OpenRegisterPage()
    {
        LoginPage.SetActive(false);
        RegisterPage.SetActive(true);
        RecoveryPage.SetActive(false);
        VerifMailPage.SetActive(false);
    }

    public void OpenRecoveryPage()
    {
        LoginPage.SetActive(false);
        RegisterPage.SetActive(false);
        RecoveryPage.SetActive(true);
        VerifMailPage.SetActive(false);
    }

    public void OpenVerifMailPage()
    {
        LoginPage.SetActive(false);
        RegisterPage.SetActive(false);
        RecoveryPage.SetActive(false);
        VerifMailPage.SetActive(true);
    }

    #endregion

    public void RegisterUser()
    {

        if (PasswordRegisterInput.text.Length < 6)
        {
            MessageText.color = Color.red;
            MessageText.text = "Password is too short! It must be at least 6 characters.";
            return; // Arrête l'exécution si le mot de passe est trop court.
        }
        else
        {
            if (PasswordRegisterInput.text.Trim() == PasswordConfirmationRegisterInput.text.Trim())
            {

                var request = new RegisterPlayFabUserRequest
                {
                    DisplayName = UsernameRegisterInput.text,
                    Email = EmailRegisterInput.text,
                    Password = PasswordRegisterInput.text,

                    RequireBothUsernameAndEmail = false
                };
                PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSucces, OnError);
            }
            else
            {
                MessageText.text = "The passwords are not the same !";
            }
        }
    }

    private void OnError(PlayFabError Error)
    {
        MessageText.color = Color.red;
        MessageText.text = Error.ErrorMessage;
        Debug.Log(Error.GenerateErrorReport());
    }

    private void OnRegisterSucces(RegisterPlayFabUserResult result)
    {
        MessageText.color = Color.green;
        MessageText.text = "Account created! Sending verification code...";

        // Génère un code aléatoire
        string verificationCode = UnityEngine.Random.Range(100000, 999999).ToString();

        // Stocke dans UserData
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
        {
            { "emailVerificationCode", verificationCode }
        }
        };

        PlayFabClientAPI.UpdateUserData(request, r =>
        {
            SendVerificationEmail(EmailRegisterInput.text, verificationCode); // ⬅️ Envoi de l'email
            MessageText.text = "Code sent to your email. Please enter it.";
            OpenVerifMailPage();
        }, OnError);
    }


    private void SendVerificationEmail(string email, string code)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("MMO Of The Four Arcanes", "autosendlinkformmoofg2s@gmail.com"));
        message.To.Add(new MailboxAddress("", email));
        message.Subject = "Code de vérification";

        message.Body = new TextPart("plain")
        {
            Text = $"Voici votre code de vérification : {code}"
        };
;

        using (var client = new SmtpClient())
        {
            client.LocalDomain = "localhost";
            client.Connect("smtp.gmail.com", 587, false);
            client.Authenticate("autosendlinkformmoofg2s@gmail.com", "rtwn znaq bfbs kzqs");
            client.Send(message);
            client.Disconnect(true);
        }
    }

    public void VerifyCodeMail()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
        {
            if (result.Data != null && result.Data.ContainsKey("emailVerificationCode"))
            {
                string storedCode = result.Data["emailVerificationCode"].Value;

                if (CodeVerifInput.text.Trim() == storedCode)
                {
                    // Met à jour le userData pour dire que le mail est vérifié
                    var request = new UpdateUserDataRequest
                    {
                        Data = new Dictionary<string, string> {
                        { "emailVerified", "true" }
                    }
                    };

                    PlayFabClientAPI.UpdateUserData(request, r =>
                    {
                        MessageText.color = Color.green;
                        MessageText.text = "Email verified! You can now login.";
                        OpenLoginPage();
                    }, OnError);
                }
                else
                {
                    MessageText.color = Color.red;
                    MessageText.text = "Incorrect verification code!";
                }
            }
            else
            {
                MessageText.color = Color.red;
                MessageText.text = "Verification code not found. Please register again.";
            }
        }, OnError);
    }



    private void OnEmailAdded(AddOrUpdateContactEmailResult result)
    {
        MessageText.color = Color.green;
        MessageText.text = "Verification email sent! Please check your inbox.";
        OpenLoginPage();
    }

    private void CheckEmailVerification()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
        {
            var data = result.Data;

            if (data != null && data.ContainsKey("emailVerified") && data["emailVerified"].Value == "true")
            {
                MessageText.color = Color.green;
                MessageText.text = "Login successful.";
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
            else
            {
                double lastSent = 0;

                if (data != null && data.ContainsKey("emailVerificationLastSent"))
                    double.TryParse(data["emailVerificationLastSent"].Value, out lastSent);

                double currentTime = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
                double elapsed = currentTime - lastSent;

                if (elapsed < 300)
                {
                    MessageText.color = Color.red;
                    MessageText.text = $"Please wait {Mathf.CeilToInt((float)(300 - elapsed))} seconds before requesting a new code.";
                    return;
                }

                // Nouveau code
                string code = UnityEngine.Random.Range(100000, 999999).ToString();

                PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
                {
                    Data = new Dictionary<string, string>
                {
                    { "emailVerificationCode", code },
                    { "emailVerificationLastSent", currentTime.ToString() }
                }
                }, updateResult =>
                {
                    SendVerificationEmail(EmailLoginInput.text, code);
                    MessageText.color = Color.yellow;
                    MessageText.text = "Your email is not verified. A new code has been sent.";
                    OpenVerifMailPage();
                }, OnError);
            }
        }, OnError);
    }


    private void OnUserDataSuccess(GetUserDataResult result)
    {
        if (result.Data != null && result.Data.ContainsKey("emailVerified") && result.Data["emailVerified"].Value == "true")
        {
            MessageText.color = Color.green;
            MessageText.text = "Login successful.";
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else
        {
            MessageText.color = Color.red;
            MessageText.text = "Please verify your email before logging in.";
        }
    }

    public void Login()
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = EmailLoginInput.text,
            Password = PasswordLoginInput.text
            // On ne demande plus InfoRequestParameters ici
        };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSucces, OnError);
    }

    private void OnLoginSucces(LoginResult result)
    {
        // Dès que le login est réussi, on vérifie dans les UserData si le mail est vérifié
        CheckEmailVerification();
    }


    public void Recovery()
    {
        var request = new SendAccountRecoveryEmailRequest
        {
            Email = EmailRecoveryInput.text,
            TitleId = "C360A",
        };
        PlayFabClientAPI.SendAccountRecoveryEmail(request, OnRecoverySucces, OnErrorRecovery);
    }

    private void OnErrorRecovery(PlayFabError result)
    {
        MessageText.text = "Email send ! Make sure to enter the good email";
        OpenLoginPage();
    }

    private void OnRecoverySucces(SendAccountRecoveryEmailResult result)
    {
        MessageText.text = "Email send ! Make sure to enter the good email";
        OpenLoginPage();
    }
}
