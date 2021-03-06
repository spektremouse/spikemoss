﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Spikemoss.ViewModels;

namespace Spikemoss.Views
{
    /// <summary>
    /// Interaction logic for ClusterView.xaml
    /// </summary>
    public partial class ClusterView : UserControl
    {
        private ClusterViewModel vm;

        public ClusterView()
        {
            InitializeComponent();
        }

        private void Vm_ErrorOccurred(object sender, EventArgs e)
        {
            var errWin = new ErrorWindow(vm);
            errWin.ShowDialog();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            vm = this.DataContext as ClusterViewModel;
            vm.ErrorOccurred += Vm_ErrorOccurred;
        }
    }
}
