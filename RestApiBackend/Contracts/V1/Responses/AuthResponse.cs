namespace RestApiBackend.Contracts.V1.Responses
{
    public class AuthResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }

        public AuthResponse()
        {}

        public AuthResponse(string token)
        {
            Token = token;
        }
    }
}