using System;
using System.Text.Json.Serialization;

namespace ApiTexPact.Models
{
    public class ItemInContainerDTO
    {
        
        public int itemInContainerId { get; set; }
        
        public string ItemCode { get; set; }
        
        public int ContainerId { get; set; }
        
        public DateTime DateTimeIn { get; set; }
        
        public DateTime DateTimeOut { get; set; }
    }
}