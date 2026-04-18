using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Movie_Catalog_Test.DTOs
{
    public class ApiResponseDTO
    {
        [JsonProperty("msg")]
        public string Msg { get; set; }

        [JsonProperty("movie")]
        public MovieDTO Movie { get; set; } = new MovieDTO();

    }
}
