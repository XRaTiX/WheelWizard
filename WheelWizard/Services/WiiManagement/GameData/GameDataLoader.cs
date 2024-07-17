﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CT_MKWII_WPF.Models.GameData;
using CT_MKWII_WPF.Utilities.Configuration;
using CT_MKWII_WPF.Utilities.Generators;

namespace CT_MKWII_WPF.Services.WiiManagement.GameData;

public class GameDataLoader
{
    private Models.GameData.GameData _gameData;
    private byte[] _saveData;
    
    public Models.GameData.GameData GameData => _gameData;
    
    private const int RksysSize = 0x2BC000;
    private const string RksysMagic = "RKSD0006";
    private const int MaxPlayerNum = 4;
    private const int RkpdSize = 0x8CC0;
    private const string RkpdMagic = "RKPD";
    private const int MaxFriendNum = 30;
    private const int FriendDataOffset = 0x56D0;
    private const int FriendDataSize = 0x1C0;
    private const int MiiSize = 0x4A;
    
    
    public GameDataLoader()
    {
        _gameData = new Models.GameData.GameData();
    }
    
    
    public void LoadGameData()
    {
        _saveData = LoadSaveDataFile();
        if (_saveData == null || _saveData.Length != RksysSize || !ValidateMagicNumber())
        {
            throw new InvalidDataException("Invalid save file data");
        }
        ParseUsers();
    }
    
    private void ParseUsers()
    {
        for (int i = 0; i < MaxPlayerNum; i++)
        {
            int rkpdOffset = RksysMagic.Length + i * RkpdSize;
            if (Encoding.ASCII.GetString(_saveData, rkpdOffset, RkpdMagic.Length) == RkpdMagic)
            {
                User user = ParseUser(rkpdOffset);
                _gameData.Users.Add(user);
            }
        }
    }
    
    private User ParseUser(int offset)
    {
        User user = new User
        {
            Name = GetUtf16String(offset + 0x14, 10),
            FriendCode = FriendCodeGenerator.GetFriendCode(_saveData,offset + 0x5C),
            MiiData = Convert.ToBase64String(_saveData.AsSpan(offset + 0x4C, MiiSize)),
            Vr = BitConverter.ToUInt16(_saveData, offset + 0xB0),
            Br = BitConverter.ToUInt16(_saveData, offset + 0xB2),
            TotalRaceCount = BitConverter.ToInt32(_saveData, offset + 0xB4)
        };

        ParseFriends(user, offset);
        return user;
    }
    
    private void ParseFriends(User user, int userOffset)
    {
        int friendOffset = userOffset + FriendDataOffset;
        for (int i = 0; i < MaxFriendNum; i++)
        {
            int currentOffset = friendOffset + i * FriendDataSize;
            if (CheckMiiData(currentOffset + 0x1A))
            {
                Friend friend = new Friend
                {
                    Name = GetUtf16String(currentOffset + 0x1C, 10),
                    FriendCode = FriendCodeGenerator.GetFriendCode(_saveData, currentOffset + 4),
                    Wins = BitConverter.ToUInt16(_saveData, currentOffset + 0x14),
                    Losses = BitConverter.ToUInt16(_saveData, currentOffset + 0x12),
                    MiiData = Convert.ToBase64String(_saveData.AsSpan(currentOffset + 0x1A, MiiSize))
                };
                user.Friends.Add(friend);
            }
        }
    }
    
    private bool CheckMiiData(int offset)
    {
        for (int i = 0; i < MiiSize; i++)
        {
            if (_saveData[offset + i] != 0) return true;
        }
        return false;
    }
    
    private string GetUtf16String(int offset, int maxLength)
    {
        List<byte> bytes = new List<byte>();
        for (int i = 0; i < maxLength * 2; i += 2)
        {
            byte b1 = _saveData[offset + i];
            byte b2 = _saveData[offset + i + 1];
            if (b1 == 0 && b2 == 0) break;
            bytes.Add(b1);
            bytes.Add(b2);
        }
        return Encoding.BigEndianUnicode.GetString(bytes.ToArray());
    }
    
    private string GetFriendCode(int offset)
    {
        // This is a simplified version. The actual friend code generation is more complex.
        uint pid = BitConverter.ToUInt32(_saveData, offset);
        return pid.ToString("D12").Insert(4, "-").Insert(9, "-");
    }

    
    private bool ValidateMagicNumber()
    {
        return Encoding.ASCII.GetString(_saveData, 0, RksysMagic.Length) == RksysMagic;
    }

    public static byte[] LoadSaveDataFile()
    {
        var saveFileLocation = Path.Combine(ConfigValidator.GetLoadPathLocation(), "Riivolution", "RetroRewind6", "save");
        var saveFile = Directory.GetFiles(saveFileLocation, "rksys.dat", SearchOption.AllDirectories);
        if (saveFile.Length == 0)
        {
            return null;
        }
        return File.ReadAllBytes(saveFile[0]);
    }
}