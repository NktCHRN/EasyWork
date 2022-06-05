import { Component, OnInit } from '@angular/core';
import { GeneralinfoService } from '../services/generalinfo.service';
import { ContentItem } from '../shared/content-item';
import { GeneralInfo } from '../shared/generalinfo';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {

  content: ContentItem[] = 
  [
    {
      heading: 'Easy to start',
      text: 'Just sign in using your <strong>Google</strong> account, '+
      'create your project or join to already created and start working with your team. ' +
      'No trainings, learning docs or anything else. Just start. <em>A piece of cake!</em>',
      image: {
        name: 'main-cake.png',
        alt: 'a piece of cake'
      }
    },
    {
      heading: 'Concentrate on work',
      text: '<strong>No more</strong> annoying notifications, hundreds or even thousands ' +
      'of messages and no advertisement. <em>First things first</em> - focus on what you really want to do ' + 
      'effectively.',
      image: {
        name: 'main-notifications.png',
        alt: 'no notifications/messages'
      }
    },
    {
      heading: 'Powerful statistic tools',
      text: '<em>Keep track</em> of your and your team\'s performance. ' +
      'With our service, you can easily watch your progress, most effictive members of your team and ' + 
      'build <strong>Gantt</strong> charts for your projects.',
      image: {
        name: 'main-gantt.png',
        alt: 'Gantt chart'
      }
    },
  ];

  info: GeneralInfo | undefined | null;

  constructor(private generalInfoService: GeneralinfoService) { }

  ngOnInit(): void {
    this.generalInfoService.getInfo()
    .subscribe({
      next: info => this.info = info,
      error: err => console.log(err)
    })
  }

}
