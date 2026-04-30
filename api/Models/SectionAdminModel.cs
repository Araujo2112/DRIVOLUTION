using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiTexPact.Models;

public class SectionAdminModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int EmployeeId { get; set; }
    public int PlantFloorSectionId { get; set; }

    public EmployeeModel Employee { get; set; }
    public PlantFloorSectionModel PlantFloorSection { get; set; }

    public DateTime AssignedDate { get; set; }
}