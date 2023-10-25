namespace _9GagClone.Helpers;

public class AuthHelper
{
    public static byte[] CreatePasswordHash(string password, string pepper)
    {
        if (password == null) throw new ArgumentNullException(nameof(password));
        if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password cannot be empty or whitespace only string.", nameof(password));

        // Combine the password with your "pepper"
        var pepperedPassword = $"{password}{pepper}";

        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            // Return the hash as a byte array
            return sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(pepperedPassword));
        }
    }
    
    public static bool VerifyPasswordHash(string password, byte[] storedHash, string pepper)
    {
        if (password == null) throw new ArgumentNullException(nameof(password));
        if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password cannot be empty or whitespace only string.", nameof(password));

        var pepperedPassword = $"{password}{pepper}";
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            var computedHash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(pepperedPassword));
            return computedHash.SequenceEqual(storedHash); // Using System.Linq;
        }
    }
    
}