﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CT_MKWII_WPF.Services.Networking;
using CT_MKWII_WPF.Services.WheelWizard;
using CT_MKWII_WPF.Views.Components;
using CT_MKWII_WPF.Views.Pages;
using MahApps.Metro.IconPacks;

namespace CT_MKWII_WPF.Views;

public partial class Layout : Window
{
    public Layout()
    {
        InitializeComponent();
        DataContext = this;
        NavigateToPage(new Dashboard());
        LiveAlertsManager.Start(UpdateLiveAlert); // Temporary code
        RRLiveRooms.Start(); // Temporary code
        RRLiveRooms.SubscribeToRoomsUpdated(UpdatePlayerAndRoomCount); // Temporary code
        
        // loadplayerdata();
    }

    // public static void loadplayerdata()
    // {
    //     var data = new GameDataLoader();
    //     data.LoadGameData();
    //     var gameData = data.GameData;
    // }

    public void NavigateToPage(Page page)
    {
        ContentArea.Navigate(page);
        ContentArea.NavigationService.RemoveBackEntry();

        foreach (var child in SidePanelButtons.Children)
        {
            if (child is SidebarRadioButton button)
                button.IsChecked = button.PageType == page.GetType();
        }
    }
    
    private void UpdatePlayerAndRoomCount()
    {
        var playerCount = RRLiveRooms.PlayerCount;
        var roomCount = RRLiveRooms.RoomCount;
        PlayerCountBox.Text = playerCount.ToString();
        PlayerCountBox.TipText = playerCount switch
        {
            1 => "There is currently 1 player online",
            0 => "There are currently no players online",
            _ => $"There are currently {playerCount} players online"
        };
        RoomCountBox.Text = roomCount.ToString();
        RoomCountBox.TipText = roomCount switch
        {
            1 => "There is currently 1 room active",
            0 => "There are currently no rooms active",
            _ => $"There are currently {roomCount} rooms active"
        };
    }

    private void UpdateLiveAlert(string messageType, string message)
    {
        if ((string)LiveAlertToolTip.Content == message) return;
        LiveAlertToolTip.Content = message;
        LiveAlert.IconPack = "FontAwesome";
        SolidColorBrush? brush = null;
        switch (messageType.ToLower())
        {
            case "party":
                LiveAlert.IconPack = "Material";
                LiveAlert.IconKind = PackIconMaterialKind.PartyPopper;
                brush = Application.Current.FindResource("PartyColor") as SolidColorBrush;
                break;
            case "info":
                LiveAlert.IconKind = PackIconFontAwesomeKind.CircleInfoSolid;
                brush = Application.Current.FindResource("InfoColor") as SolidColorBrush;
                break;
            case "warning":
            case "alert":
                LiveAlert.IconKind = PackIconFontAwesomeKind.CircleExclamationSolid;
                brush = Application.Current.FindResource("WarningColor") as SolidColorBrush;
                break;
            case "error":
                LiveAlert.IconKind = PackIconFontAwesomeKind.CircleXmarkSolid;
                brush = Application.Current.FindResource("ErrorColor") as SolidColorBrush;
                break;
            case "success":
                LiveAlert.IconKind = PackIconFontAwesomeKind.CircleCheckSolid;
                brush = Application.Current.FindResource("SuccessColor") as SolidColorBrush;
                break;
            default:
                LiveAlert.IconPack = "";
                LiveAlert.IconKind = "";
                break;
        }

        // We never actually want Brushes.Gray,
        // but just in case the resource is missing for some unknown reason, we still want to display the icon
        LiveAlert.ForegroundColor = brush ?? Brushes.Gray;
    }
    
    private void TopBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left) 
            DragMove();
    }
    
    private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private void Discord_Click(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = "https://discord.gg/vZ7T2wJnsq",
            UseShellExecute = true
        });
    }

    private void Github_Click(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = "https://github.com/patchzyy/WheelWizard",
            UseShellExecute = true
        });
    }
 
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        LiveAlertsManager.Stop(); // Temporary code
        RRLiveRooms.Stop(); // Temporary code
        RRLiveRooms.UnsubscribeToRoomsUpdated(UpdatePlayerAndRoomCount); // Temporary code
    }
}