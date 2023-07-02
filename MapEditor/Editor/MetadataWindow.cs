using Editor.Celeste;
using System;
using System.Windows.Forms;

namespace Editor
{
    public partial class MetadataWindow : Form
    {
        public MetadataWindow()
        {
            InitializeComponent();

            introTypeComboBox.Items.AddRange(Enum.GetNames(typeof(IntroType)));
        }
    }
}
