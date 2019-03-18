import { Component } from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Chart} from 'chart.js';
import {ReportingService} from './reporting.service';
import {mergeMap, groupBy, map, reduce} from 'rxjs/operators';
import {of} from 'rxjs';
 
@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent {
  title = 'Act6Ang';
  chart=[];
  courses:object=null;
  options = [
    {id: 1,text:"On Site Courses"},
    {id: 2,text:"Online Courses"},
    {id: 3,text:"All Courses"}
  ]
  Selection:number =3;
  constructor(private reporting: ReportingService){}
  random_rgba(){
    var o=Math.round, r=Math.random, s =255;
    return 'rgba('+ o(r()*s)+','+o(r()*s)+','+o(r()*s)+',0.7)';
  }
  submitRequest(){
    this.reporting.getReportingData(this.Selection).subscribe(response=>{
      console.log(response);
      let keys = response["Departments"].map(d=>d.Name);
      let values = response["Departments"].map(d=>d.Average);

      this.courses = response['Courses'];

      this.chart = new Chart('canvas',{
        type : 'bar',
        data : {
          labels :keys,
          datasets: [
            {
              data: values,
              borderColor:"#3cba9f",
              fill: false,
              backgroundColor:[
                this.random_rgba(),
                this.random_rgba(),
                this.random_rgba(),
                this.random_rgba()
              ]
            }
          ]
        },
        options:{
          legend:{
            display: false
          },
          title: {
            display: true,
            text: "Average grade by Department"
          },
          scales:{
            xAxes: [{
              display:true,
              barPercentage: 0.75
            }],
            yAxes: [{
              display: true,
              ticks:{
                min: 0,
                max:4
              }
            }],
          }
        }
      })
    });
  }
  downloadRequest(type){
    this.reporting.downloadReport(this.Selection, type).subscribe(x=>{
      var fileType = type==1?"application/pdf":"application/msword";
      var fileName = type==1?"teport.pdf":"report.doc";
      var newBlob = new Blob([x],{type:fileType});
      if (window.navigator && window.navigator.msSaveOrOpenBlob){
        window.navigator.msSaveOrOpenBlob(newBlob);
        return
      }
      const data = window.URL.createObjectURL(newBlob);

      var link = document.createElement('a');
      link.href =data;
      link.download = fileName;
      link.dispatchEvent(new MouseEvent('click',{bubbles:true,cancelable:true,view:window}));

      setTimeout(function(){
        window.URL.revokeObjectURL(data);
        link.remove;
      },100)
    })
  }
}
