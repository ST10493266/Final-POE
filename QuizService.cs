using System.Collections.Generic;

namespace CybersecurityAwarenessBotGUI;

public sealed class QuizService
{
    public List<QuizQuestion> Questions { get; } = new()
    {
        new QuizQuestion { Question = "What should you do if you receive an email asking for your password?", Options = new() { "Reply with your password", "Delete the email only", "Report the email as phishing", "Forward it to friends" }, CorrectIndex = 2, Explanation = "Reporting phishing helps prevent scams." },
        new QuizQuestion { Question = "A strong password should be:", Options = new() { "Your birthday", "A short word", "Long, unique, and hard to guess", "The same on every account" }, CorrectIndex = 2, Explanation = "Long and unique passwords reduce the chance of account compromise." },
        new QuizQuestion { Question = "Two-factor authentication adds an extra layer of account security.", Options = new() { "True", "False" }, CorrectIndex = 0, Explanation = "2FA requires another proof of identity besides the password." },
        new QuizQuestion { Question = "Which URL is safer to use when entering personal details?", Options = new() { "A misspelled website link", "A site using HTTPS and the correct domain", "A random shortened link", "A link from an unknown sender" }, CorrectIndex = 1, Explanation = "HTTPS and the correct domain are important checks before entering personal details." },
        new QuizQuestion { Question = "Social engineering attacks try to trick people into giving away information.", Options = new() { "True", "False" }, CorrectIndex = 0, Explanation = "Social engineering manipulates human trust rather than only attacking technology." },
        new QuizQuestion { Question = "What should you do before clicking an unexpected attachment?", Options = new() { "Open it quickly", "Check the sender and scan or verify it", "Disable antivirus", "Send it to everyone" }, CorrectIndex = 1, Explanation = "Unexpected attachments can contain malware, so verify them first." },
        new QuizQuestion { Question = "Using public Wi-Fi for banking is safest when:", Options = new() { "You use a trusted VPN or mobile data", "You ignore browser warnings", "You share the hotspot password", "You turn off security updates" }, CorrectIndex = 0, Explanation = "A trusted VPN or mobile network reduces exposure on public Wi-Fi." },
        new QuizQuestion { Question = "Software updates are important because they often fix security vulnerabilities.", Options = new() { "True", "False" }, CorrectIndex = 0, Explanation = "Updates frequently patch weaknesses attackers could exploit." },
        new QuizQuestion { Question = "If a message creates panic and demands urgent payment, it may be:", Options = new() { "A normal reminder", "A phishing or scam attempt", "A password manager", "A firewall update" }, CorrectIndex = 1, Explanation = "Scammers often use urgency to stop people from thinking carefully." },
        new QuizQuestion { Question = "It is safe to reuse one password across all accounts.", Options = new() { "True", "False" }, CorrectIndex = 1, Explanation = "Password reuse means one breach can expose many accounts." },
        new QuizQuestion { Question = "What is a good way to store many strong passwords?", Options = new() { "In a password manager", "On a public sticky note", "In a social media post", "In your email signature" }, CorrectIndex = 0, Explanation = "Password managers help create and store unique passwords securely." },
        new QuizQuestion { Question = "A firewall can help control network traffic entering or leaving a device.", Options = new() { "True", "False" }, CorrectIndex = 0, Explanation = "Firewalls help block unwanted or unsafe network connections." }
    };
}
