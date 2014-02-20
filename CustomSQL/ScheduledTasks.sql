/* Set up custom tasks for Booginhead */

/* CYOImageCleanup cleans up uploaded images, proofs, etc. created during CYO process */
insert into ScheduleTask (Name, Seconds, Type, Enabled, StopOnError, LastStartUTC, LastEndUTC, LastSuccessUTC)
values ('Clean up CYO files', 3600, 'Nop.Web.Models.Custom.CYOImageCleanupTask, Nop.Web', 1, 0, null, null, null)
go

/* This process sends orders to PRIDE via SFTP */
insert into ScheduleTask (Name, Seconds, Type, Enabled, StopOnError, LastStartUTC, LastEndUTC, LastSuccessUTC)
values ('Send CYO orders to PRIDE', 3600, 'Nop.Web.Models.Custom.CYOFileTransferTask, Nop.Web', 1, 0, null, null, null)
go
