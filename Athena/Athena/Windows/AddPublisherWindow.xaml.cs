﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Athena.Data.PublishingHouses;
using Microsoft.EntityFrameworkCore;

namespace Athena.Windows {
    /// <summary>
    /// Logika interakcji dla klasy AddPublisherWindow.xaml
    /// </summary>
    public partial class AddPublisherWindow {
        public PublishingHouseView PublishingHouseView { get; set; }

        public AddPublisherWindow() {
            InitializeComponent();
            this.DataContext = this;
            PublishingHouseView = new PublishingHouseView();
        }

        private void AddPublisherToDataBase_OnClick(object sender, RoutedEventArgs e) {
            PublishingHouseView.Id = Guid.NewGuid();
            ApplicationDbContext.Instance.Entry(Mapper.Instance.Map<PublishingHouse>(PublishingHouseView)).State = EntityState.Added;
            ApplicationDbContext.Instance.SaveChanges();
            this.Close();
        }
        private void Save_Executed(object sender, ExecutedRoutedEventArgs e) {
            if (!ApplicationDbContext.Instance.PublishingHouses.Any(s => s.PublisherName.ToLower() == PublisherNameTextBox.Text.ToLower())) {
                ApplicationDbContext.Instance.Entry(new PublishingHouse() {PublisherName = PublisherNameTextBox.Text, Id = Guid.NewGuid()})
                    .State = EntityState.Added;
                ApplicationDbContext.Instance.SaveChanges();
                this.Close();
            }
            else {
                PublisherExistsTextBlock.Visibility = Visibility.Visible;
            }
        }

        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = !Validation.GetHasError(PublisherNameTextBox);
        }
    }
}