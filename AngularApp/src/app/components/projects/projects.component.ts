import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ProjectsService } from '../../services/projects.service';
import { TokenService } from '../../services/token.service';
import { ProjectReducedModel } from '../../shared/project/project-reduced.model';
import { ProjectAddComponent } from './project-add/project-add.component';

@Component({
  selector: 'app-projects',
  templateUrl: './projects.component.html',
  styleUrls: ['./projects.component.scss']
})
export class ProjectsComponent implements OnInit {

  projects: ProjectReducedModel[] | null | undefined;
  errorMessage: string | null | undefined;
  loading: boolean = true;

  constructor(private _projectsService: ProjectsService, private _tokenService: TokenService, private _dialog: MatDialog) { }

  ngOnInit(): void {
    this._projectsService.get(this._tokenService.getJwtToken()!)
    .subscribe({
      next: projects => 
      {
        this.projects = projects; 
        this.loading = false;
      },
      error: error => 
      {
        this.errorMessage = typeof error === 'string' || error instanceof String ? error : error.message; 
        this.loading = false;
      },
    });
  }

  onAddClick()
  {
    this._dialog.open(ProjectAddComponent, {
      panelClass: "dialog-responsive"
    });
  }

}
