
/*
 * 本文件由根据实体插件自动生成，请勿更改
 * =========================== */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace IntegrationAdmin
{
    public class u8_GameLocal
    {
        
        /// <summary>
        /// Id
        /// </summary>    
        [Key]
        [Required] 
        [Display(Name="Id")]
        public string Id{ get; set; }
        
        /// <summary>
        /// GameLocalCategoryId
        /// </summary>    
        [Required] 
        [Display(Name="GameLocalCategoryId")]
        public string GameLocalCategoryId{ get; set; }
        
        /// <summary>
        /// ApiId
        /// </summary>    
        [Required] 
        [Display(Name="ApiId")]
        public int ApiId{ get; set; }
        
        /// <summary>
        /// GameType
        /// </summary>    
        [Required] 
        [Display(Name="GameType")]
        public int GameType{ get; set; }
        
        /// <summary>
        /// Title
        /// </summary>    
        [Required] 
        [Display(Name="Title")]
        public string Title{ get; set; }
        
        /// <summary>
        /// Url
        /// </summary>    
        [Display(Name="Url")]
        public string Url{ get; set; }
        
        /// <summary>
        /// ImageUrl
        /// </summary>    
        [Required] 
        [Display(Name="ImageUrl")]
        public string ImageUrl{ get; set; }
        
        /// <summary>
        /// RecommendNo
        /// </summary>    
        [Required] 
        [Display(Name="RecommendNo")]
        public int RecommendNo{ get; set; }
        
        /// <summary>
        /// SortNo
        /// </summary>    
        [Required] 
        [Display(Name="SortNo")]
        public int SortNo{ get; set; }
        
        /// <summary>
        /// Remark
        /// </summary>    
        [Display(Name="Remark")]
        public string Remark{ get; set; }
        
        /// <summary>
        /// Status
        /// </summary>    
        [Required] 
        [Display(Name="Status")]
        public int Status{ get; set; }
        
        /// <summary>
        /// GameIdentify
        /// </summary>    
        [Display(Name="GameIdentify")]
        public string GameIdentify{ get; set; }
        
        /// <summary>
        /// ShowJackpots
        /// </summary>    
        [Required] 
        [Display(Name="ShowJackpots")]
        public int ShowJackpots{ get; set; }
        
        /// <summary>
        /// JackpotsInfo
        /// </summary>    
        [Display(Name="JackpotsInfo")]
        public int? JackpotsInfo{ get; set; }
        
        /// <summary>
        /// JackpotsParams
        /// </summary>    
        [Display(Name="JackpotsParams")]
        public string JackpotsParams{ get; set; }
        
        /// <summary>
        /// EnTitle
        /// </summary>    
        [Display(Name="EnTitle")]
        public string EnTitle{ get; set; }
        
        /// <summary>
        /// GameNameId
        /// </summary>    
        [Display(Name="GameNameId")]
        public string GameNameId{ get; set; }
        
        /// <summary>
        /// PaymentLineNumber
        /// </summary>    
        [Display(Name="PaymentLineNumber")]
        public int? PaymentLineNumber{ get; set; }
        
        /// <summary>
        /// IsNew
        /// </summary>    
        [Required] 
        [Display(Name="IsNew")]
        public int IsNew{ get; set; }
        
        /// <summary>
        /// IsHot
        /// </summary>    
        [Required] 
        [Display(Name="IsHot")]
        public int IsHot{ get; set; }
        
        /// <summary>
        /// IsTry
        /// </summary>    
        [Required] 
        [Display(Name="IsTry")]
        public int IsTry{ get; set; }
        
        /// <summary>
        /// IsUseMobileSite
        /// </summary>    
        [Display(Name="IsUseMobileSite")]
        public bool? IsUseMobileSite{ get; set; }
        
        /// <summary>
        /// H5GameIdentify
        /// </summary>    
        [Display(Name="H5GameIdentify")]
        public string H5GameIdentify{ get; set; }
    }
}
