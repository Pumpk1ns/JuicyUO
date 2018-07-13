﻿#region license
//  Copyright (C) 2018 JuicyUO Development Community on Github
//
//	This project is an alternative client for the game Ultima Online.
//	The goal of this is to develop a lightweight client considering 
//	new technologies such as DirectX (MonoGame included). The foundation
//	is originally licensed (GNU) on JuicyUO and the JuicyUO Development
//	Team. (Copyright (c) 2015 JuicyUO Development Team)
//    
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using JuicyUO.Core.Network;
using JuicyUO.Ultima.Data;

namespace JuicyUO.Ultima.Network.Server.GeneralInfo {
    /// <summary>
    /// Subcommand 0x14: A context menu.
    /// </summary>
    class ContextMenuInfo : IGeneralInfo {
        public readonly ContextMenuData Menu;

        public ContextMenuInfo(PacketReader reader) {
            reader.ReadByte(); // unknown, always 0x00
            int subcommand = reader.ReadByte(); // 0x01 for 2D, 0x02 for KR
            Menu = new ContextMenuData(reader.ReadInt32());
            int contextMenuChoiceCount = reader.ReadByte();
            for (int i = 0; i < contextMenuChoiceCount; i++) {
                int iUniqueID = reader.ReadUInt16();
                int iClilocID = reader.ReadUInt16() + 3000000;
                int iFlags = reader.ReadUInt16(); // 0x00=enabled, 0x01=disabled, 0x02=arrow, 0x20 = color
                int iColor = 0;
                if ((iFlags & 0x20) == 0x20) {
                    iColor = reader.ReadUInt16();
                }
                Menu.AddItem(iUniqueID, iClilocID, iFlags, iColor);
            }
        }
    }
}
