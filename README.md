# COMP5348
### Create the Books, Bank, and Deliveries databases along with their associated tables.
### Enable MSMQ Server
#### 1. Open Control Panel.
#### 2. Switch the view to Category.
#### 3. Click on the Programs.
#### 4. Then proceed with Programs and Features.
#### 5. On the left pane, you will find the option Turn Windows features on or off.
#### 6. Windows features dialog box will pop-up now.
#### 7. Locate Microsoft Message Queue(MSMQ) Server from the list.
#### 8. Enable it by clicking on the checkbox. Finally, Click on OK.
#### 9. Windows will search for the required files.
#### 10. Changes will be made and it will take some time.
#### 11. Finally, Click on Close.
#### 12. On the desktop, right-click My Computer, and then click Manage.
#### 13. Expand the Services and Applications node to find Message Queuing.
#### 14. Expand Message Queuing, right-click Private Queues, point to New, and then click Private Queue.In the Queue name box, type bookstoremessage, and then click OK. 
### Start each of the applications by right clicking on the following projects, and then clicking on Debug > Start New Instance : Bank.Process ; DeliveryCo.Process ; EmailService.Process ; BookStore.Process. 
### You can then also start the BookStore web client by right clicking on BookStore.WebClient and then clicking on Debug > Start New Instance. Remember to login with the username “Customer” and the password “COMP5348” (both without the quotes).
