﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Tennis_Betfair.Properties {
    using System;
    
    
    /// <summary>
    ///   Класс ресурса со строгой типизацией для поиска локализованных строк и т.д.
    /// </summary>
    // Этот класс создан автоматически классом StronglyTypedResourceBuilder
    // с помощью такого средства, как ResGen или Visual Studio.
    // Чтобы добавить или удалить член, измените файл .ResX и снова запустите ResGen
    // с параметром /str или перестройте свой проект VS.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Возвращает кэшированный экземпляр ResourceManager, использованный этим классом.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Tennis_Betfair.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Перезаписывает свойство CurrentUICulture текущего потока для всех
        ///   обращений к ресурсу с помощью этого класса ресурса со строгой типизацией.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Поиск локализованного ресурса типа System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap green {
            get {
                object obj = ResourceManager.GetObject("green", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на function suscribeEventsScores() {
        ///    if ($(&apos;.yellow.last.ml13-ScoreBoardColumn&apos;).length == 0)
        ///        return false;
        ///    if ($(&apos;.ml13-Anims_H3Text&apos;).length == 0)
        ///        return false;
        ///    $(&apos;.yellow.last.ml13-ScoreBoardColumn&apos;).bind(&quot;DOMSubtreeModified&quot;,function(){
        ///        var players = $(&apos;.ml13-ScoreBoard_HeaderText&apos;);
        ///        var playerOneName = players[0].textContent;
        ///        var playerTwoName = players[1].textContent;
        ///        var scores = $(&apos;.yellow.last.ml13-ScoreBoardColumn&apos;).children();
        ///    [остаток строки не уместился]&quot;;.
        /// </summary>
        internal static string jsAllLoad {
            get {
                return ResourceManager.GetString("jsAllLoad", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на function getMarkets() { 
        ///    try {
        ///        console.log(&quot;Я тут&quot;);
        ///        var jsonObj = [];
        ///        $(&apos;#content &gt; div:nth-child(41) &gt; div:nth-child(2)&apos;).find(&quot;li&quot;).each(function() 
        ///        {
        ///                var playersName = jQuery.trim(this.textContent).split(&quot; v &quot;);
        ///                var player1Name = playersName[0];
        ///                var player2Name = playersName[1];
        ///                var symbol = &quot;*&quot;;
        ///                if (player1Name.indexOf(symbol) &gt; -1)
        ///                {
        ///                    player [остаток строки не уместился]&quot;;.
        /// </summary>
        internal static string jsAllLoadSkybet {
            get {
                return ResourceManager.GetString("jsAllLoadSkybet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Поиск локализованного ресурса типа System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap loader_transparent {
            get {
                object obj = ResourceManager.GetObject("loader_transparent", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Поиск локализованного ресурса типа System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap red {
            get {
                object obj = ResourceManager.GetObject("red", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Поиск локализованного ресурса типа System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap tennis_promo {
            get {
                object obj = ResourceManager.GetObject("tennis_promo", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Поиск локализованного ресурса типа System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap yellow {
            get {
                object obj = ResourceManager.GetObject("yellow", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
    }
}
