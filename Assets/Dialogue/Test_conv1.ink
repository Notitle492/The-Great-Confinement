->Chapter1
=== Chapter1 ===
# name:NPC 
# portrait:npc_無表情
你認識他嗎？　
* 認識->Yes1
* 不認識->End1

= Yes1
# name:NPC 
# portrait:npc_無表情
怎麼認識的？ 
*以前同班過，但不熟-> End2 
*之前透過朋友認識的-> End3
*他是我兒時玩伴，最近還吃過飯呢-> End4

= End1 
# portrait:clear
# name:End1
（第一結局：無憂）你佯裝無知，毫無負擔地過完了餘生。 -> END 

= End2 
# portrait:clear
# name:End2
（第二結局：生機）因為對方相信你與被害者不熟的說詞，你成功躲過追查。-> END

= End3
# portrait:clear
# name:End3
（第三結局：布局）後來你又給了NPC許多假線索，使調查陷入困境。 -> END

= End4
# portrait:clear
# name:End4
（第四結局：嫌疑）你被認定為受害者的利害關係人，被偽裝成功的NPC警方強制押送調查。-> END


- 回答結束 

-> END





