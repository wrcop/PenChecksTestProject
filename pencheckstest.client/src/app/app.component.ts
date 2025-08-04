import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { FormGroup, FormControl, Validators } from '@angular/forms';

class Customer {
  id: number = 0;
  name: string = "";
  accounts: AccountInfo[] = [];
}

class AccountInfo {
  accountId: number = 0;
  balance: number = 0;
  type: string = "";
  transactionHistory: TransactionItem[] = [];
}

class TransactionItem {
  id: number = 0;
  type: TransactionType = TransactionType.Deposit;
  transferAccount: number = 0;
  amount: number = 0;
  balance: number = 0;
  timeStamp: Date = new Date();
}

class TransactionRequest {
  Id: number = 0;
  Type: number = -1;
  TransferAccount: number = 0;
  Amount: number = 0;
  Balance: number = 0;
  constructor() {
    this.Type = 0;
  }
}

class RequestStatus {
  error: boolean = false;
  errorInformation: string = "";
  transaction: TransactionItem = new TransactionItem();
}

enum TransactionType {
  Deposit = 'Deposit',
  Withdrawl = 'Withdrawl',
  TransferTo = 'Transfer to account',
  TransferFrom = 'Transfer from account'
}


@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: false,
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  public customer: Customer = new Customer();
  public account: AccountInfo = new AccountInfo();
  public transaction: TransactionRequest = new TransactionRequest();
  public errorStatus: RequestStatus = new RequestStatus();
  constructor(private http: HttpClient) {}


  ngOnInit() {
    
  }

  getCustomerInfo(customerId: number) {
    this.http.post<Customer>('/Account/GetCustomerInfo', { CustomerId: customerId }).subscribe(
      (data) => {
        this.customer = data;
      },
      (error) => {
        console.error(error);
        //handle & display error message to user here
      }
    )
  }

  getAccountInfo(accountId:number) {
    this.http.post<AccountInfo>('/Account/GetAccountBalance', { AccountId: accountId }).subscribe(
      (data) => {
        this.account = data;
        //reset transaction information
        this.transaction.Id = this.account.accountId;
        this.transaction.Type = 0;
        this.transaction.Amount = 0.00;
        //reset error message
        this.errorStatus = new RequestStatus();
      },
      (error) => {
        console.error(error);
        //handle & display error message to user here
        this.errorStatus.error = true;
        this.errorStatus.errorInformation += " " + error;
      }
    )
  }

  submitTransaction() {
    //if type is transfer, grab other account number for submission
    //in a more robust app there would be a drop-down to select the account to accomodate more than 1 other account and this would not be needed
    var transferAcct = 0;
    if (this.transaction.Type == 3) {
      if (this.customer.accounts[0].accountId == this.account.accountId) {
        transferAcct = this.customer.accounts[1].accountId;
      }
      else {
        transferAcct = this.customer.accounts[0].accountId;
      }
      
    }
    //fix because the Type field keeps being turned into a string unwillingly at runtime, breaking the .NET object parsing in the controller
    var TypeFix = 0;
    switch (this.transaction.Type.toString()) {
      case "0": TypeFix = 0; break;
      case "1": TypeFix = 1; break;
      case "2": TypeFix = 2; break;
      case "3": TypeFix = 3; break;
    }

    this.http.post<RequestStatus>('/Account/ProcessTransaction', { Id: this.transaction.Id, Type: TypeFix, TransferAccount: transferAcct, Amount: this.transaction.Amount, Balance: this.transaction.Balance }).subscribe(
      (data) => {
        this.errorStatus = data;
        //this.transaction = new TransactionRequest();
        if (!this.errorStatus.error) {
          this.getAccountInfo(this.account.accountId);
        }        
      },
      (error) => {
        console.error(error);
        //handle & display error message to user here
        this.errorStatus.error = true;
        this.errorStatus.errorInformation += " " + error;
      }
    )
    

  }

  title = 'pencheckstest.client';
}
