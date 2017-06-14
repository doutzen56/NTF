
/*
 * 本文件由根据实体插件自动生成，请勿更改
 * =========================== */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace UnitTest
{
    public class Customers
    {
        
        /// <summary>
        /// CustomerID
        /// </summary>    
        [Key]
        [Required] 
        [StringLength(100)]
        [Display(Name="CustomerID")]
        public string CustomerID{ get; set; }
        
        /// <summary>
        /// ContactName
        /// </summary>    
        [StringLength(100)]
        [Display(Name="ContactName")]
        public string ContactName{ get; set; }
        
        /// <summary>
        /// Phone
        /// </summary>    
        [StringLength(100)]
        [Display(Name="Phone")]
        public string Phone{ get; set; }
        
        /// <summary>
        /// City
        /// </summary>    
        [StringLength(100)]
        [Display(Name="City")]
        public string City{ get; set; }
        
        /// <summary>
        /// Country
        /// </summary>    
        [StringLength(100)]
        [Display(Name="Country")]
        public string Country{ get; set; }
    }
}
