Ceva Firmware Generator
Copyright (C) 2016 Rockchip Electronics Co., Ltd.
Author: zhichao.yu@rock-chips.com
Date: 20161019
Version: V0.2.8

This tool is used to generate CEVA DSP firmware for Rockchip SOCs platforms.
Customers should obtain license from CEVA and Rockchip before use this tool.

=== How To Use This Tool ===
Following setps to generate your DSP firmware:
1) Move CEVA executable file(xx.a) to input directory
2) Use cmd to execute CevaFirmwareGenerator.exe
3) Then the firmware file is created in out directory, named "rkdsp.bin"
4) Use the new generated firmware file to replace rkdsp.bin in <SdkRoot>/external/dpp/out/firmware/
5) Rebulid SDK image

=== How To Configurate ===
CevaFirmwareGenerator has a configuration file - FwConfig.xml. You can modify this
configuration file to generate your specific firmware format if needed.
