namespace Cards.API.Entities.DTO
{
    public class ResponseDTO
    {
        public object Response { get; set; }
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }
    }
}