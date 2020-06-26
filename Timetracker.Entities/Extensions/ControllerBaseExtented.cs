namespace Timetracker.Models.Extensions
{
    using Microsoft.AspNetCore.Mvc;
    using System;

    public static class ControllerBaseExtented
    {
        public static int ParseValue( this ControllerBase controller, uint? id )
        {
            if ( !id.HasValue )
            {
                throw new Exception( "Вы не предоставили поле id" );
            }

            if ( !int.TryParse( id.Value.ToString(), out var parsedId ) )
            {
                throw new Exception( "Недопустимый формат идентификатора" );
            }

            return parsedId;
        }
    }
}
