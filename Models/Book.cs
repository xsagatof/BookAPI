﻿namespace BookAPI.Models
{
	public class Book
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public int PublicationYear { get; set; }
		public string Author { get; set; }
		public int ViewsCount { get; set; }
	}
}
