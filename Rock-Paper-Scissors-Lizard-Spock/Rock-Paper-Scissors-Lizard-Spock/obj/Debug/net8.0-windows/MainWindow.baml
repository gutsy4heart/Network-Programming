   M S B A M L   `   `   `  ����[  WRock-Paper-Scissors-Lizard-Spock, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullS.clr-namespace:Rock_Paper_Scissors_Lizard_Spock Rock_Paper_Scissors_Lizard_Spock  5   	   1    +Rock_Paper_Scissors_Lizard_Spock.MainWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   @local.clr-namespace:Rock_Paper_Scissors_Lizard_Spock     5   	     � Title$&   Rock-Paper-Scissors-Lizard-Spock��5      $	��350��61   $	��525��6>   .��5	      � .+�5
   
   �� -   5         ��Name$ MoveSelection��5
      $
��Left=�6*   $	��Top=�6E   $	��120��6]   $��	10,10,0,0q�6i   .@�5      �� .��6   Rock6!   �� 5      .��6   Paper6"   �� 5      .��6   
Scissors6%   �� 5      .��6   Lizard6#   �� 5      .��6   Spock6"   5      �� 5   
   $��	Make Move��6   $
��Left=�6%   $	��Top=�6@   $	��120��6X   $��	10,40,0,0q�6d   �� 5   
   $��
Offer Draw��6   $
��Left=�6&   $	��Top=�6A   $	��120��6Y   $��	10,80,0,0q�6e   �� 5   
   $��Concede��6   $
��Left=�6#   $	��Top=�6>   $	��120��6V   $��
10,120,0,0q�6b   �� 5   
   -   5         ��Name$ 
GameStatus��5      $
��Left=�6&   $	��Top=�6A   $	��300��6Y   $	��100��6e   $��
150,10,0,0q�6r    �� 
IsReadOnly$
 True��6�   5      5      