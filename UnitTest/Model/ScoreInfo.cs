
/*
 * 本文件由根据实体插件自动生成，请勿更改
 * =========================== */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace UnitTest
{
    public class ScoreInfo
    {
        
        /// <summary>
        /// ScoreID
        /// </summary>    
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required] 
        [Display(Name="ScoreID")]
        public int ScoreID{ get; set; }
        
        /// <summary>
        /// Score
        /// </summary>    
        [Required] 
        [Display(Name="Score")]
        public int Score{ get; set; }
        
        /// <summary>
        /// UserId
        /// </summary>    
        [Required] 
        [Display(Name="UserId")]
        public int UserId{ get; set; }
    }
}
