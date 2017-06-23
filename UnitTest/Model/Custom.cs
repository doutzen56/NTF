
/*
 * 本文件由根据实体插件自动生成，请勿更改
 * =========================== */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Test
{
    public class Custom
    {
        
        /// <summary>
        /// ID
        /// </summary>    
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required] 
        [Display(Name="ID")]
        public int ID{ get; set; }
        
        /// <summary>
        /// Name
        /// </summary>    
        [Display(Name="Name")]
        public string Name{ get; set; }
        
        /// <summary>
        /// Age
        /// </summary>    
        [Display(Name="Age")]
        public int? Age{ get; set; }
    }
}
