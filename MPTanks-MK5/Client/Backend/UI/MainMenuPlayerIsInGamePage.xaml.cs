// -----------------------------------------------------------
//  
//  This file was generated, please do not modify.
//  
// -----------------------------------------------------------
namespace EmptyKeys.UserInterface.Generated {
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.ObjectModel;
    using EmptyKeys.UserInterface;
    using EmptyKeys.UserInterface.Data;
    using EmptyKeys.UserInterface.Controls;
    using EmptyKeys.UserInterface.Controls.Primitives;
    using EmptyKeys.UserInterface.Input;
    using EmptyKeys.UserInterface.Media;
    using EmptyKeys.UserInterface.Media.Animation;
    using EmptyKeys.UserInterface.Media.Imaging;
    using EmptyKeys.UserInterface.Shapes;
    using EmptyKeys.UserInterface.Renderers;
    using EmptyKeys.UserInterface.Themes;
    
    
    [GeneratedCodeAttribute("Empty Keys UI Generator", "1.6.7.0")]
    public partial class MainMenuPlayerIsInGamePage : UIRoot {
        
        private Grid e_31;
        
        private StackPanel e_32;
        
        private TextBlock e_33;
        
        private TextBlock e_34;
        
        private Button e_35;
        
        public MainMenuPlayerIsInGamePage(int width, int height) : 
                base(width, height) {
            Style style = RootStyle.CreateRootStyle();
            style.TargetType = this.GetType();
            this.Style = style;
            this.InitializeComponent();
        }
        
        private void InitializeComponent() {
            this.Background = new SolidColorBrush(new ColorW(0, 0, 0, 255));
            FontManager.Instance.AddFont("Segoe UI", 12F, FontStyle.Regular, "Segoe_UI_9_Regular");
            InitializeElementResources(this);
            // e_31 element
            this.e_31 = new Grid();
            this.Content = this.e_31;
            this.e_31.Name = "e_31";
            // e_32 element
            this.e_32 = new StackPanel();
            this.e_31.Children.Add(this.e_32);
            this.e_32.Name = "e_32";
            this.e_32.HorizontalAlignment = HorizontalAlignment.Center;
            this.e_32.VerticalAlignment = VerticalAlignment.Center;
            // e_33 element
            this.e_33 = new TextBlock();
            this.e_32.Children.Add(this.e_33);
            this.e_33.Name = "e_33";
            this.e_33.Margin = new Thickness(20F, 20F, 20F, 0F);
            this.e_33.Foreground = new SolidColorBrush(new ColorW(255, 255, 255, 255));
            this.e_33.Text = "A game is currently active...";
            this.e_33.TextAlignment = TextAlignment.Center;
            FontManager.Instance.AddFont("Segoe UI", 48F, FontStyle.Regular, "Segoe_UI_36_Regular");
            this.e_33.FontSize = 48F;
            // e_34 element
            this.e_34 = new TextBlock();
            this.e_32.Children.Add(this.e_34);
            this.e_34.Name = "e_34";
            this.e_34.Margin = new Thickness(20F, 20F, 20F, 0F);
            this.e_34.Foreground = new SolidColorBrush(new ColorW(255, 255, 255, 255));
            this.e_34.Text = "You\'re in game right now.\nClose the game to get back to the main menu.\nOr, if you" +
                "\'re absolutely sure:";
            this.e_34.TextAlignment = TextAlignment.Center;
            FontManager.Instance.AddFont("Segoe UI", 24F, FontStyle.Regular, "Segoe_UI_18_Regular");
            this.e_34.FontSize = 24F;
            // e_35 element
            this.e_35 = new Button();
            this.e_32.Children.Add(this.e_35);
            this.e_35.Name = "e_35";
            this.e_35.Margin = new Thickness(10F, 10F, 10F, 10F);
            FontManager.Instance.AddFont("Segoe UI", 24F, FontStyle.Regular, "Segoe_UI_18_Regular");
            this.e_35.Content = "Click to forcibly close game";
            Binding binding_e_35_Command = new Binding("ForciblyCloseButtonCommand");
            this.e_35.SetBinding(Button.CommandProperty, binding_e_35_Command);
            this.e_35.SetResourceReference(Button.StyleProperty, "PrimaryButton");
        }
        
        private static void InitializeElementResources(UIElement elem) {
            elem.Resources.MergedDictionaries.Add(UITemplateDictionary.Instance);
        }
    }
}
