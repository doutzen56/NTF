
/*
 * 本文件由根据实体插件自动生成，请勿更改
 * =========================== */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace UnitTest
{
    public class Orders
    {
        
        /// <summary>
        /// OrderID
        /// </summary>    
        [Key]
        [Required] 
        [Display(Name="OrderID")]
        public int OrderID{ get; set; }
        
        /// <summary>
        /// CustomerID
        /// </summary>    
        [StringLength(100)]
        [Display(Name="CustomerID")]
        public string CustomerID{ get; set; }
        
        /// <summary>
        /// OrderDate
        /// </summary>    
        [Display(Name="OrderDate")]
        public DateTime? OrderDate{ get; set; }
    }
}
