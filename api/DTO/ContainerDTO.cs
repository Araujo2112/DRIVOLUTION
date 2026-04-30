using System.ComponentModel.DataAnnotations;
using ApiTexPact.Models;

public class ContainerDTO
{
    [Required]
    public int ContainerId { get; set; }
    
    [Required]
    public string ContainerCode { get; set; }

    [Required]
    public string ContainerName { get; set; }

    [Required]
    public float ContainerVolume { get; set; }

    [Required]
    public bool Activate { get; set; }
    
}