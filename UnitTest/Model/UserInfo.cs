
/*
 * 本文件由根据实体插件自动生成，请勿更改
 * =========================== */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Tzen
{
    public class UserInfo
    {

        /// <summary>
        /// ID
        /// </summary>    
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        [Display(Name = "ID")]
        public int ID { get; set; }

        /// <summary>
        /// Name
        /// </summary>    
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// Age
        /// </summary>    
        [Required]
        [Display(Name = "Age")]
        public int Age { get; set; } 

        /// <summary>
        /// Address
        /// </summary>    
        [Display(Name = "Address")]
        public string Address { get; set; }

    }
}
