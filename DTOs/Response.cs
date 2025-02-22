using System.ComponentModel.DataAnnotations;

namespace BookAPI.DTOs
{
	public class Response
	{
		[Required] 
		public string Status { get; set; }
		[Required] 
		public string Message { get; set; }
	}
}
