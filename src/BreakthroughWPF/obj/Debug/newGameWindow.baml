   M S B A M L   `   `   `  ����  BreakthroughWPF#    BreakthroughWPF.newGameWindow   5   	   \ XPresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      8x,http://schemas.microsoft.com/winfx/2006/xaml    5        � Title$  New game options��5      $	��124��6   $	��426��6+    � 
ResizeMode$ NoResize=�67    � WindowStartupLocation$ CenterScreen=�6M    � Icon$9 3/BreakthroughWPF;component/Resources/RookYellow.ico��6r    � ShowInTaskbar .   � Topmost . .��5      � $��87��6    � ColumnDefinitions 5   
   �� 5      $
��152* �6   �� 5      $
��191* �6    � RowDefinitions 5
   
   �� 5      $	��85* �6   �� 5      $	��32* �6   .+�5   
   �� -   5        $��okButton��5   -   $��1��6   $��160,0q�6   $��23��6=   $	��Top=�6I   $��2��6a   $��Stretch=�6u   .��6�   Begin6�   �� 5   
   -   5        $��label1��5   "   $��29,0,0,0q�6   $��28��60   $	��Top=�6<   $��Right=�6T   .��6w   Player Black (Top) :6�   �� 5   
   -   5        $��label2��5   .   $��28��6   $��8,28,0,0q�6   $	��Top=�6<   $��Right=�6T   .��6w   Player White (Bottom) :6�   	� 5   
   -   5        $��radioButton1��5   F   $��1��6   $��16��6&   $��
23,5,107,0q�62   $	��Top=�6Z    	� 	GroupName$ black��6r   	 d� 	IsChecked$
	 TrueY�6�   .��6�   
Computer6�   	� 5   
   -   5        $��radioButton2��5   H   $ black��6   $��16��6(   $��
117,5,13,0q�64   $	��Top=�6\   $��1��6t   .��6�   Human6�   	� 5   
   -   5        $��radioButton3��5   I   $ white��6   $��16��6(   $��23,33,107,0q�64   $	��Top=�6]   $��1��6u   .��6�   
Computer6�   	� 5   
   -   5        $��radioButton4��5   I   $ white��6   $��16��6(   $��117,33,13,0q�64   $	��Top=�6]   $��1��6u   $
	 TrueY�6�   .��6�   Human6�   �� 5   
   -   5        $��label3��5      $��Italic$�6   $��Center=�61   $��1��6U   $��24.445��6e   $��2��6u   $��Bottom=�6�   $��Right=�6�   $	��147��6�   .��6�   White always move first.6�   5      5      