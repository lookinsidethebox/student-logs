namespace Core.Models
{
	public class EducationMaterialOrderModel
	{
		public int CurrentMaterialId { get; set; }
		public int? PreviousMaterialId { get; set; }
		public int? NextMaterialId { get; set; }
	}
}
