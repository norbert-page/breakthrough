   M S B A M L   `   `   `  ����  BreakthroughWPF(    "BreakthroughWPF.ParticipantsWindow   5   	   \ XPresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      8x,http://schemas.microsoft.com/winfx/2006/xaml    5      -   5          � Title$  Players near you��5      $	��366��6   $	��300��6+    � WindowStartupLocation$ CenterOwner=�67    � Topmost .   � ShowInTaskbar .   � 
ResizeMode$ CanResizeWithGrip=�6�    � Icon$9 3/BreakthroughWPF;component/Resources/RookYellow.ico��6�   .��5      �  � ColumnDefinitions 5   
   �� 5      $��18.18* �6   �� 5      $��240.885* �6   �� 5	      $��18.935* �6    � RowDefinitions 5   
   �� 5      $��26.361* �6   �� 5      $��35.639* �6   �� 5      $
��167* �6   �� 5      $	��82* �6   �� 5      $	��17* �6   .+�5   
   �� -   5        $��statusLabel��5      $��3��6#   $��Center=�67   $��Center=�6[   $��2q�6}   $��2q�6�   
�����_�$��Offline��5   3   ��5      �� 5       �� 
StartPoint$	 0,0&�6&   	 �� EndPoint$		 1,1&�67   
 �� Opacity$
 1Q�6F   .��5      �  � Color$ Lavender��6#    � Offset$	 0.2Q�64   � 5      $ LightSkyBlue��6#   $	 1.0Q�68   5      5      �� 5   
   -   5        $��cancelButton��5   .   $��1��6   $��3��6!    �� IsCancel . $��30.749��6R   $��Bottom=�6b   $��165.885,0,0,0q�6}   $
��Left=�6�   $��75��6�   .��6�   Cancel6�   �� 5   
   -   5        $��inviteButton��5   H   $��1��6   $��3��6!   $��83.628,0,0,0.701q�6.    �� 	IsDefault . $��58.097��6�   $��Bottom=�6�   $
��Left=�6�   $��74.538��6�   .��6�   Invite6�   �� 5   
   -   5        $��playersListBox��5   1   $��1��6   $��0,9,0,27q�6   $��1��6G   $��2��6W   �� 5   
   -   5        $��restartButton��5   C   $��1��6   $��3��6!   $��0,0,0,0.701q�6.   $��30.048��6t   $��Bottom=�6�   $
��Left=�6�   $��75��6�   .��6�   	Restart6�   �� 5   
   -   5        $��discoveryLabel��5      $��28��6   $��Bottom=�62   $��3��6M   $��Center=�6a   $��Center=�6�    J� Opacity$ 0Q�6�   $��Normal"�6�   $��2��6�   .��6�   )'performing continuous players discovery6�   �� 5    
   -   5        $��
rectangle1��5    @   $��3��6   $��3��6(   $��1��65   
�������$	��Top=�6b   5!      5"      