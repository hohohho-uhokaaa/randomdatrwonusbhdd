# randomdatrwonusbhdd
9.6GB random data write/read throughput speed test
Grok と 日曜日の午後の暇つぶしバイブ・コーディング
HP pavillion 190 Ryzen 5 2400G + orico DD18U3 USB 3.0 SATA3 HDD dock + DT01ACA100 1TB 7200rpm SATA3 HDD
9.6GB ランダムデータの書き出しと読み込み 1TB の SATA3 HDD 遊ばせてるのもあれなので
read/wite のバッファは 128MB とかがいいらしい HP 190-0204jp はフロントが USB 3.1 Gen1 ✗ 4
dotnet build 初回の build は 6 秒くらい あれこれ build してるから秒で終わる

$ lsusb -t
/: Bus 001.Port 001: Dev 001, Class=root_hub, Driver=xhci_hcd/4p, 480M
    |__ Port 003: Dev 002, If 0, Class=Hub, Driver=hub/4p, 480M
        |__ Port 001: Dev 009, If 0, Class=Human Interface Device, Driver=usbhid, 12M
        |__ Port 001: Dev 009, If 1, Class=Human Interface Device, Driver=usbhid, 12M
        |__ Port 003: Dev 007, If 0, Class=Video, Driver=uvcvideo, 480M
        |__ Port 003: Dev 007, If 1, Class=Video, Driver=uvcvideo, 480M
        |__ Port 003: Dev 007, If 2, Class=Audio, Driver=snd-usb-audio, 480M
        |__ Port 003: Dev 007, If 3, Class=Audio, Driver=snd-usb-audio, 480M
        |__ Port 004: Dev 011, If 0, Class=Human Interface Device, Driver=usbhid, 1.5M
        |__ Port 004: Dev 011, If 1, Class=Human Interface Device, Driver=usbhid, 1.5M
    |__ Port 004: Dev 003, If 0, Class=Wireless, Driver=btusb, 12M
    |__ Port 004: Dev 003, If 1, Class=Wireless, Driver=btusb, 12M
/: Bus 002.Port 001: Dev 001, Class=root_hub, Driver=xhci_hcd/4p, 10000M
/: Bus 003.Port 001: Dev 001, Class=root_hub, Driver=xhci_hcd/1p, 480M
    |__ Port 001: Dev 002, If 0, Class=Hub, Driver=hub/4p, 480M
/: Bus 004.Port 001: Dev 001, Class=root_hub, Driver=xhci_hcd/1p, 10000M
    |__ Port 001: Dev 002, If 0, Class=Hub, Driver=hub/4p, 5000M
        |__ Port 003: Dev 003, If 0, Class=Mass Storage, Driver=uas, 5000M

現在の状況
速度: 5000M（USB 3.1 Gen1 / USB 3.0）
Driver: uas（最適なドライバ）

期待できる速度（この構成の場合）
書き込み: 70〜110 MB/s 前後
読み込み: 110〜160 MB/s 前後
（Toshiba DT01ACA100 + Orico DD18U3 + USB 3.0）

$ dotnet build
復元が完了しました (1.5 秒)
  random10gb net10.0 成功しました (0.8 秒) → bin/Debug/net10.0/random10gb.dll
2.9 秒後に 成功しました をビルド
 $ dotnet run
=== 9.6GB USB 3.0 速度テスト ===
【書き込みテスト開始】
書き込み中... 9.60 GB | 164.4 MB/s
✅ 書き込み完了 → 161.3 MB/s
【読み込みテスト開始】
読み込み中... 9.50 GB | 177.4 MB/s
✅ 読み込み完了 → 177.3 MB/s
テスト完了！

✅ 大成功です！
素晴らしい速度が出ましたね！
今回の結果
書き込み速度: 171.6 MB / s
読み込み速度: 183.9 MB / s
評価
USB 3.0 + Orico DD18U3 + DT01ACA100 の組み合わせとして非常に良い数値です。
7200rpm HDDのUSB 3.0経由としてはほぼ上限に近い優秀な結果です。

へーそうなんや

$ dotnet run
=== 9.00 GB USB 3.0 速度テスト ===
【書き込みテスト開始】
書き込み中... 9.50 GB | 181.5 MB/ss
✅ 書き込み完了 → 164.3 MB/s
【読み込みテスト開始】
読み込み中... 9.60 GB | 231.2 MB/s
✅ 読み込み完了 → 231.2 MB/s
テスト完了！
