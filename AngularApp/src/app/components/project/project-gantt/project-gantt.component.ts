import { AfterViewInit, Component, ElementRef, Inject, OnInit, ViewChild } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Title } from '@angular/platform-browser';
import { first, Observable, ReplaySubject } from 'rxjs';
import { GanttService } from 'src/app/services/gantt.service';
import { ProjectService } from 'src/app/services/project.service';
import { TimeService } from 'src/app/services/time.service';
import { ConnectionContainer } from 'src/app/shared/other/connection-container';
import { GanttModel } from 'src/app/shared/project/gantt/gantt.model';
import { Month } from 'src/app/shared/project/gantt/month';
import { UserOnProjectRole } from 'src/app/shared/project/user-on-project/role/user-on-project-role';
import { UserOnProjectReducedModel } from 'src/app/shared/project/user-on-project/user-on-project-reduced.model';
import { ProjectGanttInfoComponent } from './project-gantt-info/project-gantt-info.component';

@Component({
  selector: 'app-project-gantt',
  templateUrl: './project-gantt.component.html',
  styleUrls: ['./project-gantt.component.scss']
})
export class ProjectGanttComponent implements OnInit, AfterViewInit {
  projectId: number = undefined!;
  projectName: string = undefined!;
  me: UserOnProjectReducedModel = undefined!;
  userOnProjectRoles = UserOnProjectRole;

  connectionContainer: ConnectionContainer = new ConnectionContainer();

  startDate: Date;
  endDate: Date = new Date();
  months: Month[] = undefined!;

  model: GanttModel = undefined!;
  errorMessage: string | null | undefined;
  isErrorCritical = false;
  loading: boolean = true;
  loaded: ReplaySubject<boolean> = new ReplaySubject(1);

  @ViewChild('ganttContainer') private ganttContainer: ElementRef = undefined!;

  constructor(private _titleService: Title, @Inject('projectName') private _websiteName: string, private _ganttService: GanttService,
  private _projectService: ProjectService, private _dialog: MatDialog, private _timeService: TimeService) {
    this.startDate = new Date(this.endDate.getTime() - this._timeService.daysToMilliseconds(365));
   }

  ngAfterViewInit(): void {
    this.loaded.pipe(first())
    .subscribe(value => {
      if (value)
        setTimeout(() => this.scrollToTheEnd(), 500);
    });
  }

  scrollToTheEnd(): void {
    try {
      this.ganttContainer.nativeElement.scrollLeft  = this.ganttContainer.nativeElement.scrollWidth - this.ganttContainer.nativeElement.clientWidth;
    } catch(err) { }                 
  }

  private convertStartDate(): void
  {
    this.startDate.setHours(0);
    this.startDate.setMinutes(0);
    this.startDate.setSeconds(0);
    this.startDate.setMilliseconds(0);
  }

  private setMonths(): void
  {
    this.months = this._ganttService.getMonths(this.startDate, this.endDate);
  }

  ngOnInit(): void {
    this._titleService.setTitle(`${this.projectName} | Gantt chart - ${this._websiteName}`);
    this.loaded.subscribe(() => this.loading = false);
    this._projectService.getById(this.projectId)
    .subscribe({
      next: result => 
      {
        const projectStartDate = new Date(result.startDate);
        if (projectStartDate > this.startDate)
        {
          if (new Date().getTime() - projectStartDate.getTime() <= this._timeService.daysToMilliseconds(1))
          {
            this.errorMessage = "Your project should have been existing for at least one day to generate a Gantt chart";
            this.loaded.next(false);
            return;
          }
          this.startDate = projectStartDate;
        }
        this.convertStartDate();
        this.setMonths();
        this._ganttService.get(this.projectId, this.startDate, this.endDate)
        .subscribe(
          {
            next: result => 
            {
              this.model = result;
              this.loaded.next(true);
            },
            error: error => {
              this.isErrorCritical = true;
              const message = `An error occured: ${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`;
              this.errorMessage = message;
              console.error(message);
              this.loaded.next(false);
            }
          }
        );
      },
      error: error =>
      {
        this.isErrorCritical = true;
        const message = `An error occured: ${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`;
        this.errorMessage = message;
        console.error(message);
        this.loaded.next(false);
      }
    });
  }

  openInfoDialog(): void
  {
    this._dialog.open(ProjectGanttInfoComponent, {
      panelClass: "dialog-responsive"
    });
  }
}
