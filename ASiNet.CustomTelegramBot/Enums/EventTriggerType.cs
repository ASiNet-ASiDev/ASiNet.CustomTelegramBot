﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASiNet.CustomTelegramBot.Enums;
public enum EventTriggerType
{ 
    None,
    Message,
    Command,
    ButtonCallback,
}