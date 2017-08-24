
/*
 * 本文件由根据实体插件自动生成，请勿更改
 * =========================== */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace IntegrationAdmin
{
    public class u8_Update_Jackpot
    {
        
        /// <summary>
        /// GamePlatform
        /// </summary>    
        [Key]
        [Required] 
        [Display(Name="GamePlatform")]
        public string GamePlatform{ get; set; }
        
        /// <summary>
        /// GameIdentify
        /// </summary>    
        [Key]
        [Required] 
        [Display(Name="GameIdentify")]
        public string GameIdentify{ get; set; }
        
        /// <summary>
        /// JackpotsInfo
        /// </summary>    
        [Required] 
        [Display(Name="JackpotsInfo")]
        public int JackpotsInfo{ get; set; }
        
        /// <summary>
        /// JackpotsParams
        /// </summary>    
        [Display(Name="JackpotsParams")]
        public string JackpotsParams{ get; set; }
        
        /// <summary>
        /// Amount
        /// </summary>    
        [Required] 
        [Display(Name="Amount")]
        public decimal Amount{ get; set; }
        
        /// <summary>
        /// IntervalTime
        /// </summary>    
        [Required] 
        [Display(Name="IntervalTime")]
        public int IntervalTime{ get; set; }
        
        /// <summary>
        /// IntervalLength
        /// </summary>    
        [Required] 
        [Display(Name="IntervalLength")]
        public decimal IntervalLength{ get; set; }
        
        /// <summary>
        /// UpdateTime
        /// </summary>    
        [Required] 
        [Display(Name="UpdateTime")]
        public DateTime UpdateTime{ get; set; }
    }
}
