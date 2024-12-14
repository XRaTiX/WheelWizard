﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Interactivity;
using System;

namespace WheelWizard.Views.Components;

public class Button : Avalonia.Controls.Button // Change to TemplatedControl
{
    public static readonly StyledProperty<ButtonsVariantType> VariantProperty =
        AvaloniaProperty.Register<Button, ButtonsVariantType>(nameof(Variant), ButtonsVariantType.Default);

    public static readonly StyledProperty<Geometry> IconDataProperty =
        AvaloniaProperty.Register<Button, Geometry>(nameof(IconData));

    public static readonly StyledProperty<double> IconSizeProperty =
        AvaloniaProperty.Register<Button, double>(nameof(IconSize), 20.0);

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<Button, string>(nameof(Text));
    
    public enum ButtonsVariantType
    {
        Primary,
        Warning,
        Default,
        Danger
    }
    
    // Constructor
    public Button()
    {
        // No need for InitializeComponent() in code-behind for TemplatedControl
        UpdateStyleClasses(Variant);
    }

    // Properties remain the same
    public ButtonsVariantType Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public Geometry IconData
    {
        get => GetValue(IconDataProperty);
        set => SetValue(IconDataProperty, value);
    }

    public double IconSize
    {
        get => GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    
    // UpdateStyleClasses remains the same
    private void UpdateStyleClasses(ButtonsVariantType variant)
    {
        Classes.Remove("Primary");
        Classes.Remove("Warning");
        Classes.Remove("Danger");
        Classes.Remove("Default");

        switch (variant)
        {
            case ButtonsVariantType.Primary:
                Classes.Add("Primary");
                break;
            case ButtonsVariantType.Warning:
                Classes.Add("Warning");
                break;
            case ButtonsVariantType.Danger:
                Classes.Add("Danger");
                break;
            default:
                Classes.Add("Default");
                break;
        }
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == VariantProperty)
            UpdateStyleClasses(change.GetNewValue<ButtonsVariantType>());
    }
}
